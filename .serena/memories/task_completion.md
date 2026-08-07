# Task Completion Checklist

Before finishing a code task:
- Run the narrowest meaningful build/test command for the changed project(s).
- For analyzer changes, build the analyzer project and run the matching analyzer test project.
- Check `git status --short` and make sure only intentional files were changed.
- Do not revert user changes or unrelated dirty worktree files.
- Report any tests that could not be run and why.

For new projects:
- Add project files to `neonSDK.slnx` when the project should participate in solution workflows.
- Respect Central Package Management and shared build props.
- Add required license/header comments to new source files.