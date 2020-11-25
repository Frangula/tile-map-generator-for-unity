# tile-map-generator-for-unity
Generator of 2d tile maps and their navigation graphs for Unity from pre-made parts.
## Installation
Download the Unity Package and import it to your Unity project.
## Usage
For now the generator works only with maps in JSON format, created in the Tiled Map Editor or having a similar structure, with custom parameters of the weights of the map edges, in which each layer is stored in a `Base64` string compressed via `Zlib`, `Gzip` or `Zstd`.
The maps are connected according to the following principle: if maps A and B have the same edge weight on opposite sides, then they can be connected. If there are many matching B maps for one map A, then the map to connect is randomly selected.
For now the directory with map files is hardcoded and looks like:
`Assets/Resources/MapFiles/%SceneName%/`
## Requirements
 - Newtonsoft.JSON >= 12.0.3
 - ZstdNet >= 1.3.3.0
 - libzstd >= 1.4.0
