[![Build Status](https://travis-ci.org/ManticSic/ChangelogGenerator.svg?branch=master)](https://travis-ci.org/ManticSic/ChangelogGenerator)

# ChangelogGenerator
Simple changelog generator using milestones and associated pull requests.

## CLI options

### ChangelogGenerator-help
Display the help screen.

### ChangelogGenerator-version
Display version information.

### ChangelogGenerator-new
Generate a new or override a existing changelog file.

```
--exclude-unknown    (Default: false) Exclude pull requests without milestones

--owner              Required. Set the owner of the repository.

--repository         Required. Set the repository name.

--token              GitHub authentication token.

-o, --output         (Default: CHANGELOG.md) Set output file name.

-v, --verbose        (Default: false) Be verbose.
```

### ChangelogGenerator-generate
Generate a changelog for a specific milestone.

```
--milestone      Required. Title of the milestone.

--owner          Required. Set the owner of the repository.

--repository     Required. Set the repository name.

--token          GitHub authentication token.

-o, --output     (Default: CHANGELOG.md) Set output file name.

-v, --verbose    (Default: false) Be verbose.
```



