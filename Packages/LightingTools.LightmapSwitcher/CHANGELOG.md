# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2019.3.0] - 2021-03-23

### This is the first release of Lightmap switching tool package on openUPM.

*Short description of this release*

## [2019.3.1] - 2021-03-25

- Replace readme
- Add sample
- Start changelog

## [2019.4.1] - 2024-08-23

- Optimizations by @Bian-Sh
- Fix issues when applying lightmap scale and offset
- Fix issues after deleting lighting data asset from the scene
- Finish scriptable asset workflow. You can now Create a lighting data asset in the Project window and use it inside asset bundles.

## [2019.4.2] - 2024-08-26

- Fix for terrain/multiple terrains

## [2019.4.3] - 2024-08-31

- Remove the need to manually store the lighting scenario data
- Fix lightmap indices storage and application at runtime

## [2019.4.4] - 2024-09-08

- Fixed issue with null lighting scenario name
- Fixed issue when several gameobjects have the same name and same transform, they couldn't be matched by the script to assign lightmap scale offset.