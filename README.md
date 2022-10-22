# Geometry Dash Texture Swapper
Geometry Dash Texture Swapper, a program that allows you to swap your current Geometry Dash textures with other ones in a fast and efficient way!
This is windows only.

## First time:
When opening GDTS for the first time, and if no Default Texture Pack exists, it will ask to get all files (excluding .dat) from the Geometry Dash Resources Folder. So if you already have a texture pack, you might want to download the Default Texture Pack from the release page and put the folder in the Texture Packs Folder instead. 
Now add all your GD texture pack folders into the Texture Packs Folder and click Refresh Texture Packs.

## How to use
You can add texture packs to the selected list, like this:

![AddTexturePack](https://raw.githubusercontent.com/YaEnergy/GDTS/master/Assets/Github/AddTexturePack.gif)

And remove texture packs from the selected list, like this:

![RemoveTexturePack](https://raw.githubusercontent.com/YaEnergy/GDTS/master/Assets/Github/RemoveTexturePack.gif)

The files in the lowest texture pack are prioritized first, then the texture pack above, and so on.

The Apply Texture button applies all selected texture packs to your GD Resource Folder.
The Refresh Texture Packs button removes all texture packs from the selected and list, and updates the available texture packs list.
The RESET DEFAULT button will overwrite all files in the Default Texture with all files from your Geometry Dash Resource Folder. (Excluding .dat files)

<details>
  <summary><h2>Possible Known Errors</h2></summary>
  1. If you get an error stating that GDTS can not find your Geometry Dash folder. Open the GDResourceFolderPath.txt file. This file contains the path to your Geometry Dash folder (Not the resources folder in the GD Folder!!) on steam. It uses a default path first, but this may not always be the correct path. If this is the case, the program will error and you'll have to go to the .txt file and update it with the correct path.
</details>
  
 If you find any errors, please make an issue, so we can fix the problems together! :)