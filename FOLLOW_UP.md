# FOLLOW_UP

This plan tracks the remaining architectural cleanup after implementing items #2 and #3.

## 1) Replace Global Game Access with Explicit Injection

### Remaining smell
- Runtime code now uses `Game.*` directly instead of `GameServicesLocator`, but this is still global state.
- Many classes still have hidden dependencies and require runtime bootstrap to test.

### Breaking change
- Move runtime classes from static `Game` access to explicit constructor/factory-injected interfaces.

### Plan
1. Add composition-root helpers that construct nodes/services with explicit dependencies.
2. Inject interfaces into gameplay/UI/runtime classes instead of reading `Game.*` from inside methods.
3. Restrict `Game` usage to bootstrap and composition boundary code only.

### Acceptance criteria
- Non-bootstrap production classes do not access `Game.*`.
- Runtime dependencies are explicit in constructors/factories.
- Unit tests can instantiate classes with fakes without initializing autoload runtime.
