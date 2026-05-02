# Versionize Workflow

This project uses [Versionize](https://github.com/versionize/versionize) for automated semantic versioning based on conventional commits.

## Installation

Versionize is installed globally:
```bash
dotnet tool install --global Versionize
```

Make sure `~/.dotnet/tools` is in your PATH:
```bash
export PATH="$PATH:~/.dotnet/tools"
```

## Commit Message Format

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>: <description>

[optional body]

[optional footer(s)]
```

### Types

| Type | Version Bump | Example |
|------|--------------|---------|
| `feat:` | Minor (1.0.0 → 1.1.0) | `feat: add cache invalidation` |
| `fix:` | Patch (1.0.0 → 1.0.1) | `fix: resolve null reference` |
| `perf:` | Patch | `perf: optimize cache lookup` |
| `BREAKING CHANGE:` | Major (1.0.0 → 2.0.0) | `feat!: remove deprecated API` |
| `docs:` | No bump | `docs: update README` |
| `refactor:` | No bump | `refactor: simplify factory` |
| `chore:` | No bump | `chore: update dependencies` |

## Workflow

### 1. Make Changes
```bash
# Work on your feature
git add .
git commit -m "feat: add distributed cache support"
git commit -m "fix: handle connection timeout"
```

### 2. Bump Version (Dry Run)
```bash
versionize --dry-run
```

This shows what would happen without making changes.

### 3. Bump Version (For Real)
```bash
versionize
```

This will:
- Analyze commits since last tag
- Bump version in all `.csproj` files listed in `.versionize`
- Update `CHANGELOG.md`
- Create a git commit with message like `chore(release): 1.2.0`
- Create a git tag `v1.2.0`

### 4. Push
```bash
git push
git push --tags
```

## Manual Version Bump

Force a specific version:
```bash
versionize --release-as 2.0.0
```

## Configuration

See `.versionize` for project-specific settings:
- Which `.csproj` files to update
- Changelog sections and visibility
- Commit message format

## Tips

- **Always use conventional commits** - Otherwise Versionize won't know what version to bump
- **Run `--dry-run` first** - See what will happen before committing
- **Don't manually edit versions** - Let Versionize manage them
- **Tag is the source of truth** - Version comes from git tags, not `.csproj`
