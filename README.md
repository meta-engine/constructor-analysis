# Runtime Constructor Property Matching

Reflection describes a constructor's parameters. It does not tell a code generator
which public properties retain those arguments. This small C# reference explores
that missing connection: supply distinctive values, construct an object, then match
its public property values to the supplied arguments.

For DTO-shaped types, those observations can help associate a constructor parameter
with property metadata even when their names differ or one argument reaches several
properties. The companion article, [Runtime Constructor Analysis](https://www.metaengine.eu/articles/runtime-constructor-analysis),
walks through the example and an interactive visualization.

This is a standalone playground for that technique. MetaEngine uses related
observations within a larger code-generation pipeline; this repository does not
reproduce its production implementation or compatibility contract.

## Run it

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download), then:

```bash
git clone https://github.com/meta-engine/constructor-analysis.git
cd constructor-analysis
dotnet run --project src/Demo.ConsoleApp
```

The app prints three examples. Change the types under
[`src/Demo.ConsoleApp/Examples`](src/Demo.ConsoleApp/Examples) and run it again.

The entry point is `ConstructorFlowAnalyzer`. For the article's `User` type:

```csharp
using ConstructorAnalysis;
using Demo.ConsoleApp.Examples;

var analysis = new ConstructorFlowAnalyzer().Analyze(typeof(User));

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

Here `userName` matches both `User.Username` and `BaseEntity.Name`. That is the
useful result for property-oriented code generation: one parameter, two observed
property destinations.

## How matching works

The analyzer creates the whole argument vector before invoking the selected
constructor once. It then reads public instance properties with public getters,
excluding indexers, and compares their values with the supplied sentinels.

- Strings use ordinal equality; value types use value equality.
- Class, array, and interface sentinels use reference identity. Concrete classes use
  uninitialized instances, while interfaces use a `DispatchProxy`.
- Nullable parameters use the underlying value strategy. Unsupported shapes and
  colliding or exhausted sentinel domains receive explicit outcomes. `null` is never
  accepted as property-matching evidence.
- If a string parameter has no exact property matches, complete-sentinel containment
  with ordinal case-insensitive comparison can produce a `Heuristic` match with
  `TransformedStringContainment` provenance. Case changes and surrounding text can
  match this way; truncation, hashing, and other transformations that discard the
  sentinel cannot.

`Exact` with `ExactSentinel` provenance means **the observed property value matched
the supplied sentinel exactly**. It does not establish that every input follows the
same path, or distinguish every assignment from a coincidental constant. Numeric
sentinels are deterministic and enum sentinels come from declared members: a
constant `int.MaxValue` or the selected enum member can match an ignored parameter.
Use the observations in the context of the type being generated.

`Analyze(type)` chooses the instance constructor with the most parameters, including
non-public constructors. A metadata-token tie-break makes equal-sized choices
deterministic. Use `AnalyzeConstructor(type, constructor)` to select a particular
constructor. A type with no discoverable instance constructor returns no analysis.

### Parameter outcomes

| Outcome | Meaning |
| --- | --- |
| `Inferred` | At least one exact or transformed-string property match was observed. Inspect each mapping's confidence and provenance. |
| `Unmatched` | Construction and property inspection succeeded, but no readable property matched the supported sentinel. |
| `Ambiguous` | The sentinel cannot distinguish this parameter safely, for example for booleans or colliding finite-domain values. |
| `Unsupported` | The parameter shape has no supported sentinel strategy, such as custom structs and some abstract or delegate references. |
| `InstantiationFailed` | Constructor binding or execution failed; the detail describes the failure. |
| `InspectionFailed` | Construction succeeded, but a public getter could not be read. The detail identifies the property and failure; no partial property mappings are returned. |

A single boolean observation cannot distinguish an assignment from a constant, so
boolean parameters are always ambiguous in this one-vector sample. Unsupported
parameters keep their own outcome even if another stage fails.

## Optional direct-base metadata

Property matching also sees inherited public properties. A separate probe can add
an association with an immediate base-constructor parameter; this extra metadata
does not change the original property match.

The base probe runs only when the immediate base has exactly one constructor
accessible to the derived type and that constructor has parameters. Correlating the
same reflected property in the derived and base probes produces an ordered base
parameter candidate. Multiple accessible base constructors produce an ambiguous
base outcome instead of selecting an overload.

For simple assignment-based DTO constructors, a get-only auto-property provides
useful supporting evidence: its private, compiler-generated `initonly` field rules
out an ordinary write from a derived constructor. If both property observations
are exact, the sample labels the base candidate `Inferred`, `Exact`, and
`ReadOnlyBaseSentinel`. Writable properties keep the `Ambiguous`, `Heuristic`,
`DirectBasePropertyCorrelation` labels.

**The sample does not verify the simple-assignment precondition.** Read-only state
alone does not establish which base parameter received the argument. A base
constructor that chooses between its parameters may take different paths in the
two probes and produce an incorrect inferred slot. Conditional or transformed
constructor behavior, and constants that collide with sentinels, therefore limit
this metadata. `ReadOnlyBaseSentinel` records the two observations and property
shape; it is not a general proof of the constructor call's argument flow.

The analyzer examines one base level. It does not reconstruct constructor syntax,
trace a complete inheritance chain, or resolve overloaded base calls. A failing
base getter is reported separately as an `InspectionFailed` direct-base outcome,
while a successful original property observation remains available.

## Boundaries

- Constructors **and property getters execute**, including their side effects.
  Keep experiments on types whose execution is safe in the current environment.
- Generated values can violate a type's validation rules or encounter missing
  external state. Failures are reported; the sample does not retry with other
  values.
- Only properties with public getters are observed. Private state and field-only
  assignments are outside the sample's scope, while computed getters are included.
- Matching observes one generated input vector. It is useful metadata for known,
  simple types, not a general static analysis or a guarantee of semantic intent.
- The direct-base read-only check assumes ordinary verifiable code. Reflection or
  unverifiable IL can write an `initonly` field.

## Build and check

```bash
dotnet restore constructor-analysis.sln
dotnet build constructor-analysis.sln --configuration Release --no-restore --warnaserror
dotnet test constructor-analysis.sln --configuration Release --no-build
```

The xUnit suite covers renamed and one-to-many mappings, reference sentinels,
constructor selection, transformed strings, uncertain and unsupported inputs,
constructor and getter failures, direct-base examples, and console transcripts.
[GitHub Actions](.github/workflows/ci.yml) runs the same Release build and tests on
pushes and pull requests to `main`.

## Repository map

- [`src/ConstructorAnalysis`](src/ConstructorAnalysis) — matching logic and result models
- [`src/Demo.ConsoleApp`](src/Demo.ConsoleApp) — three runnable examples
- [`tests/ConstructorAnalysis.Tests`](tests/ConstructorAnalysis.Tests) — regression tests and fixtures

## License

This standalone repository is licensed under the [MIT License](LICENSE).
