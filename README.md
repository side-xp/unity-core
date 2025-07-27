# SideXP Core package

Core SideXP library for Unity, used as a dependency by most of our other packages.

As a general rule, this package is meant to be used as a foundation for our other packages, and can be imported safely in any project. It follows the following recommendations:

- not being tied to a specific Render Pipeline
- not alter the way Unity works or its expected usage by default
- not have dependencies to our other packages
- not have dependencies to specific Unity packages or third-party libraries

## Features

- Utility functions
- Useful extensions for both Unity and C# types
- Custom attributes
- Design pattern implementations
- Editor behavior monitoring
- Legacy GUI helpers

## Installation

### Option 1: Using the Package Manager

1. In your *Unity* project, go to `Window > Package Management > Package Manager` (or `Window > Package Manager` for *Unity 6.0-*)
2. Click on the *+* icon in the top-left corner, and select *Install package from Git URL...*
3. In the text field, enter the URL to this package's repository (including the `*.git` extensions), and click *Install*
4. Wait for Unity to get the files, and you're ready to go!

> Tip: if you need to use a specific version of this package for your project, add `#<tag-name>` to the URL before clicking on the *Install* button.

### Option 2: Extracting archive manually

1. Go to this project's `/releases` list
2. Download the ZIP file archive of your desired version
3. Extract the content of that archive into the `Packages/` folder of your Unity project
4. Wait for Unity to reload the solution, and you're ready to go!

> Tip: to avoid any path issue, make sure the folder that contains the package content has the same name as the `name` property defined in its `package.json` file.

## Documentation & Help

Complete documentation available at https://side-xp.github.io/core

If you need help or just want to chat with the community and the *Sideways Experiments* core team, you're welcome to join our [Discord server](https://discord.gg/bMK2d47JaE)!

## Contributing

Do you want to get involved in our projects? Check the [CONTRIBUTING.md](./.github/CONTRIBUTING.md) file to learn more!

## License

This project is licensed under the [MIT License](./LICENSE.md).

---

Crafted and maintained with love by [Sideways Experiments](https://sideways-experiments.com)

(c) 2022-2025 Sideways Experiments