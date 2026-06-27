# Probability Lists

Properties of type `ProbabilityList<T>` (where `T` is a serializable type) are decorated with a property drawer that displays the items of a list with a float slider that represents their probability to be picked, when using the `Get()` function.

You can use the `[ProbabilityCollectionOptions]` attribute to configure options like if elements are reorderable in the inspector, or if the list allow items to be added/removed.

You can also use the `ProbabilityEnum<T>` (where `T` is an enum type) type to get the same display, but for each item of the given enum type.