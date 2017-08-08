# FutureProofOfConcept

A covariant async/await type.

This is a simple project serving as a proof-of-concept for a covariant type that can be created with `async` and consumed by `await`.

This is not production-ready; in particular:

- All the compiler support types are public and dumped in the main namespace.
- This approach uses cheap-and-dirty reflection over some BCL types, which can break with future updates.
- I haven't tested whether `ConfigureAwait(false)` actually *works*. Just got it to compile and run without exceptions.
- I've only done the most basic tests.

I did run into some limitations where `dynamic`-based reflection fails at runtime with misleading errors, particularly when generics are in play. These are all marked in the code with the comment `// Dynamic doesn't work here! :(`
