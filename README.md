# Runtime Constructor Flow Analysis

This repository is a compact standalone reference for discovering how constructor
arguments appear in public instance properties. It places distinctive runtime
sentinels in the argument vector, invokes a constructor, and inspects the resulting
object. That belongs to the established family of sentinel-based dynamic taint and
data-flow techniques; the interesting application here is constructor-mapping
metadata, not a claim of a new analysis theorem or algorithm.

The canonical repository is `meta-engine/constructor-analysis`. It is currently
private while the companion MetaEngine website articles await owner approval. This
code is deliberately smaller than the production system: MetaEngine adds configuration
metadata, dependency injection, target-parameter normalization, semantic constructor
models, and renderer integration. Those bounded distinctions explain the reference's
scope without claiming implementation parity, compatibility, or shared internals.

## Run it

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download). The console
app is the playground: clone, run, then change the examples under
`src/Demo.ConsoleApp/Examples`.

```bash
dotnet restore constructor-analysis.sln
dotnet build constructor-analysis.sln --configuration Release --no-restore --warnaserror
dotnet test constructor-analysis.sln --configuration Release --no-build
dotnet run --project src/Demo.ConsoleApp --configuration Release --no-build
```

The public entry point is `ConstructorFlowAnalyzer`:

```csharp
var analysis = new ConstructorFlowAnalyzer().Analyze(typeof(Customer));

foreach (var parameter in analysis!.ParameterMappings)
{
    Console.WriteLine($"{parameter.Parameter.Name}: {parameter.Outcome}");
    foreach (var property in parameter.PropertyMappings)
    {
        Console.WriteLine(
            $"  -> {property.Property.Name} " +
            $"({property.Confidence}, {property.Provenance})");
    }
}
```

`Analyze(type)` selects the instance constructor with the most parameters, including
non-public constructors; a metadata-token tie-break makes equal-sized choices
deterministic. It does not combine results from every overload. Call
`AnalyzeConstructor(type, constructor)` when a particular `ConstructorInfo` should be
examined instead.

## How the probe works

For the selected constructor, the analyzer:

1. creates a sentinel for every parameter before invoking anything;
2. marks sentinel shapes that are unsupported, ambiguous, exhausted, or equal to
   another argument so they cannot produce an inferred mapping;
3. invokes the constructor once with the complete attempted all-distinct argument
   vector; and
4. reads public, non-indexed instance properties and correlates their values with the
   sentinels.

This keeps selected-constructor invocation count constant instead of invoking once per
parameter. It also avoids filling non-probed positions with `null`, which lets common
null-guarded constructors run. The direct-base check described below may perform one
additional probe of the base constructor.

Exact string and value-type mappings use value equality. Class, array, and interface
sentinels are assignable instances and are matched by reference identity: concrete
classes use an uninitialized instance, while interfaces use a `DispatchProxy`.
Nullable parameters use the underlying value strategy. Generated scalar and enum
values avoid the default where the type's domain allows it. `null` is never accepted
as flow evidence, and a collision between generated arguments makes both parameters
ambiguous instead of guessing.

Strings have one deliberately lower-confidence path. If strict ordinal equality finds
nothing, a property containing the complete sentinel with ordinal case-insensitive
comparison is reported with `Heuristic` confidence and
`TransformedStringContainment` provenance. This can recognize case changes,
surrounding concatenation, or other results that retain the complete sentinel but are
not identical to it. A pure `Trim()` of the generated whitespace-free sentinel remains
identical and therefore resolves through the exact path. Containment cannot establish
exact flow, and transforms that remove, split, reorder, truncate, encode, or hash the
sentinel remain unmatched.

### Parameter outcomes

Every parameter has an explicit `ParameterInferenceOutcome`:

| Outcome | Meaning |
| --- | --- |
| `Inferred` | One or more exact or transformed-string property mappings were found. Inspect each mapping's confidence and provenance. |
| `Unmatched` | A supported sentinel was passed successfully, but no readable property preserved it. |
| `Ambiguous` | A unique conclusion is unsafe, for example for Boolean parameters, colliding values, an enum with too few non-default values, or an exhausted narrow scalar domain. |
| `Unsupported` | The analyzer cannot create a safe assignable or collision-resistant sentinel for that shape, such as some abstract/delegate references or custom structs. |
| `InstantiationFailed` | Constructor binding, access, support, or constructor execution failed. The detail includes the observed failure. |

Boolean flow is intentionally ambiguous even for a single Boolean parameter: one
`true`/`false` observation cannot distinguish an assignment from a constant property.
The same principle applies when a finite or narrow value domain cannot provide enough
non-default distinct values. These are typed results, not silent fallback behavior.

## Direct-base candidates

The analyzer also examines the immediate `BaseType` only when it has exactly one
accessible constructor in total. Parameterless constructors participate in that count;
if the sole accessible constructor is parameterless, direct-base probing is skipped.
Otherwise, the analyzer probes that one constructor separately and correlates derived
and base observations by the identity of the reflected property (declaring type plus
property name).

Each `DirectBaseParameterMapping` exposes the candidate base parameter's ordered
`ParameterIndex`, optional `ParameterName`, correlated property, `Heuristic`
confidence, and `DirectBasePropertyCorrelation` provenance. Candidates are returned
in base-parameter order, so same-typed swapped arguments do not depend on reflection
property order.

These records are possible correlations, not confirmed `base(...)` call flow. A write
performed locally by the derived constructor can produce the same observation. When
the direct base exposes multiple callable overloads, the analyzer reports an ambiguous
direct-base outcome rather than choosing one. For these reasons candidate records use
the `Ambiguous` outcome, and the compatibility property `IsPassedToBase` remains false
unless a future strategy can establish an inferred result.

Only the direct base is examined. The analyzer does not recurse through multi-level
inheritance, reconstruct constructor call syntax, resolve overloaded base calls, or
disambiguate property shadowing and derived writes.

## Executable regression evidence

The xUnit suite contains 24 executable cases: 19 `[Fact]` cases across
[`ConstructorFlowAnalyzerTests.cs`](tests/ConstructorAnalysis.Tests/ConstructorFlowAnalyzerTests.cs)
and
[`DemoConsoleOutputTests.cs`](tests/ConstructorAnalysis.Tests/DemoConsoleOutputTests.cs),
plus five typed rows in the `[Theory]` from
[`ScalarSentinelFactoryTests.cs`](tests/ConstructorAnalysis.Tests/ScalarSentinelFactoryTests.cs).
The exact scenarios are:

- exact renamed and one-to-many mappings:
  `MapsExactAndRenamedValues`, `MapsOneParameterToMultipleProperties`;
- one-invocation and reference sentinel behavior:
  `UsesOneConstructorInvocationForNullGuardedParameters`,
  `ReportsClassAndInterfaceSentinelsAsExact`;
- transformed strings and explicit uncertainty:
  `ReportsTransformedStringMatchesAsHeuristic`,
  `ReportsBooleanCollisionAsAmbiguous`,
  `DoesNotOverclaimOneAssignedBooleanSentinel`,
  `DoesNotMistakeAnIgnoredBooleanForAConstantTrueProperty`;
- supported and unsupported shapes:
  `SupportsEnumAndNullableSentinels`,
  `ReportsCustomStructAsUnsupportedWithoutDefaultMatch`,
  `ReportsUnconstructableReferenceSentinelAsUnsupportedWithoutThrowing`;
- direct-base boundaries:
  `MapsSwappedSameTypeArgumentsToOrderedDirectBaseParameters`,
  `ReportsOverloadedDirectBaseAsAmbiguousWithoutChoosingAnOverload`,
  `ReportsDerivedWriteCorrelationAsAHeuristicCandidate`;
- selection and repeatability:
  `ReturnsNullWhenTypeHasNoConstructor`, `SelectsRichestConstructor`,
  `RepeatedRunsHaveDeterministicResults`;
- console transcript locked to the article:
  `UserTranscriptMatchesThePublishedConsoleContract`,
  `EmployeeTranscriptReportsRenamedRoleLandingAndBaseCandidate`; and
- narrow-domain exhaustion for `byte`, `sbyte`, `short`, `ushort`, and `char`:
  five rows of `ExhaustedNarrowScalarDoesNotRemainSupported`.

The fixtures used by those tests live beside them in
[`tests/ConstructorAnalysis.Tests`](tests/ConstructorAnalysis.Tests). GitHub Actions
runs on pushes and pull requests to `main`; its
[`ci.yml`](.github/workflows/ci.yml) restores the solution, performs a zero-warning
Release build with warnings treated as errors, and runs the suite without rebuilding.

## Residual limitations

- Constructor code and side effects execute. Do not analyze untrusted types or types
  whose construction is unsafe in the current environment.
- A constructor can still reject generated values or require external state. Such
  failures are reported, not bypassed.
- Types with no discoverable instance constructor return no analysis. Abstract or open
  generic targets can expose constructor metadata but fail at invocation, which is
  reported as `InstantiationFailed`; abstract classes, delegates, some interfaces, and
  custom structs may also lack a safe parameter-sentinel strategy.
- Only public readable, non-indexed instance properties are observed. Field writes and
  private-only state are outside the reference's scope.
- Runtime equality cannot prove semantic intent. Exact identity is stronger evidence
  than containment or direct-base correlation, but it is still an observation from one
  generated input vector.
- The implementation is a teaching/reference artifact, not a production compatibility
  contract for MetaEngine or its hosted services.

## Repository map

- [`src/ConstructorAnalysis`](src/ConstructorAnalysis) — analyzer and result models
- [`src/Demo.ConsoleApp`](src/Demo.ConsoleApp) — three runnable examples
- [`tests/ConstructorAnalysis.Tests`](tests/ConstructorAnalysis.Tests) — regression suite and fixtures
- [`.github/workflows/ci.yml`](.github/workflows/ci.yml) — restore/build/test workflow

## License

This standalone repository is licensed under the MIT License. See
[`LICENSE`](LICENSE). That license statement applies to this repository; it does not
describe private MetaEngine implementations or hosted services.
