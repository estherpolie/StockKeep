# Contributing to StockKeep

Thanks for your interest in improving StockKeep! This guide explains how to propose changes and what reviewers look for.

## Before you start

- **Bugs:** open an issue using the *Bug report* template. Include steps to reproduce.
- **Features:** open an issue using the *Feature request* template **before** writing code, so we can agree on the approach.
- Small fixes (typos, docs) can go straight to a pull request.

## Development workflow

1. Fork the repository and create a branch from `main`:
   - `fix/<short-description>` for bug fixes
   - `feat/<short-description>` for features
   - `docs/<short-description>` for documentation
2. Make your change. Keep each pull request focused on **one** thing.
3. Add or update tests in `tests/StockKeep.Core.Tests` for any logic change.
4. Make sure everything passes locally:
   ```bash
   dotnet build
   dotnet test
   ```
5. Open a pull request against `main` and fill in the template. Link the issue (`Fixes #12`).

## Code guidelines

- Business rules and database code belong in `StockKeep.Core`, not in the forms.
- Always use parameterised SQL (`$name`). Never build SQL by joining strings.
- The project builds with nullable reference types on and warnings treated as errors. Please don't suppress warnings without a comment explaining why.
- Follow the existing style (4-space indentation, file-scoped namespaces, `_camelCase` private fields). The `.editorconfig` file configures most editors for you.

## Review process

- Every pull request needs **one approving review from a maintainer** and a **green CI run** before it can merge.
- Reviewers check: correctness and edge cases, tests, scope (no unrelated changes), readability and consistency with the existing design.
- Expect feedback. "Request changes" is a normal part of the process, not a rejection.
- Pull requests are **squash-merged**, so each change becomes a single commit on `main`.
- Pull requests that are inactive for 30 days after a review may be closed. You're welcome to reopen them.

## Releases

Maintainers tag releases using [semantic versioning](https://semver.org/) (`v1.2.3`) and record changes in [CHANGELOG.md](CHANGELOG.md).

## Conduct

By participating you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md).
