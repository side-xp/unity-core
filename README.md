# SideXP - Core (Unity)

Core SideXP library for Unity, used as a dependency by most of our other packages.

As a general rule, this package is meant to be used as a foundation for our other packages, and can be imported safely in any project. It follows the following recommendations:

- not tied to a specific Render Pipeline
- not alter the way Unity works or its expected usage by default
- not have dependencies to our other packages
- not have dependencies to specific Unity packages or third-party libraries

## Feature Highlights

- **[Inspector attributes](https://side-xp.github.io/unity-core/guides/inspector-attributes/)**: decorate fields to change how they draw or behave, without a custom editor ([`[Readonly]`](https://side-xp.github.io/unity-core/guides/inspector-attributes/#readonly), [`[EnableIf]`](https://side-xp.github.io/unity-core/guides/inspector-attributes/#enableif-and-disableif), [`[ProgressBar]`](https://side-xp.github.io/unity-core/guides/inspector-attributes/#progressbar), [`[Remap]`](https://side-xp.github.io/unity-core/guides/inspector-attributes/#remap-and-percents), ...).
- **Type extensions**: fluent one-liners for Unity and C# types ([`TransformExtensions`](https://side-xp.github.io/unity-core/api/SideXP.Core/TransformExtensions), [`IEnumerableExtensions`](https://side-xp.github.io/unity-core/api/SideXP.Core/IEnumerableExtensions), and (in the editor) [`SerializedPropertyExtensions`](https://side-xp.github.io/unity-core/api/SideXP.Core.EditorOnly/SerializedPropertyExtensions), ...).
- **Utility classes**: focused static helpers such as [`MathUtility`](https://side-xp.github.io/unity-core/api/SideXP.Core/MathUtility), [`ColorUtility`](https://side-xp.github.io/unity-core/api/SideXP.Core/ColorUtility), [`TransformUtility`](https://side-xp.github.io/unity-core/api/SideXP.Core/TransformUtility) (pooling & hierarchy), and (in the editor) [`ObjectUtility`](https://side-xp.github.io/unity-core/api/SideXP.Core.EditorOnly/ObjectUtility), ...
- **[Weighted random](https://side-xp.github.io/unity-core/guides/probability-lists/)**: `ProbabilityList<T>`, an inspector-editable weighted collection for loot tables, spawn tables, or random events.
- **[Inline sub-assets](https://side-xp.github.io/unity-core/guides/subassets/)**: `SubassetsList<T>` lets you author nested `ScriptableObject`s straight from the parent asset's inspector.
- **Editor-friendly serializable types**: [`OptionalValue<T>`](https://side-xp.github.io/unity-core/api/SideXP.Core/OptionalValue), [`Range`/`RangeInt`](https://side-xp.github.io/unity-core/guides/range), [`Pagination`](https://side-xp.github.io/unity-core/guides/pagination), ...

... and many more things!

## Installation

### Option 1: Using the Package Manager

1. In your *Unity* project, go to `Window > Package Management > Package Manager` (or `Window > Package Manager` for *Unity 6.0-*)
2. Click on the *+* icon in the top-left corner, and select *Install package from Git URL...*
3. In the text field, enter the URL to this package's repository (including the `*.git` extension), and click *Install*
4. Wait for Unity to get the files, and you're ready to go!

> Tip: if you need to use a specific version of this package for your project, add `#<tag-name>` to the URL before clicking on the *Install* button.

### Option 2: Extracting archive manually

1. Go to this project's `/releases` list
2. Download the ZIP file archive of your desired version
3. Extract the content of that archive into the `Packages/` folder of your Unity project
4. Wait for Unity to reload the solution, and you're ready to go!

> Tip: to avoid any path issue, make sure the folder that contains the package content has the same name as the `name` property defined in its `package.json` file.

## Documentation & Help

<!-- docs:remove:start -->
Complete documentation available at https://side-xp.github.io/unity-core

<!-- docs:remove:end -->
If you need help or just want to chat with the community and the *Sideways Experiments* core team, you're welcome to join our [Discord server](https://discord.gg/bMK2d47JaE)!

## Contributing

<!-- docs:remove:start -->
Do you want to get involved in our projects? Check the [CONTRIBUTING.md](./CONTRIBUTING.md) file to learn more!
<!-- docs:remove:end -->
<!-- docs:only:start
Do you want to get involved in our projects? Check our [contributing guidelines](https://github.com/side-xp/unity-core/blob/main/CONTRIBUTING.md) to learn more!
docs:only:end -->

## License

<!-- docs:remove:start -->
This project is licensed under the [MIT License](./LICENSE.md).
<!-- docs:remove:end -->
<!-- docs:only:start
This project is licensed under the [MIT License](https://mit-license.org).
docs:only:end -->

---

Crafted and maintained with love by [Sideways Experiments](https://sideways-experiments.com)

(c) 2022-2026 Sideways Experiments
