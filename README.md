# Local Tier List

#### A simple Tier List Maker that can work without access to the internet, for both your desktop and your mobile device. <br> Mostly for tracking your opinions for yourself or to show them to your friends.

<div align="center">
<p>Main Tierlist View (Desktop)</p>
<img src="DemoImages/TierlistDesktopMinimal.jpg" style="width: 100%"></img>
  <p>Main Tierlist View (Mobile)<br>(Scale can be changed with a switch)</p>
<img src="DemoImages/TierlistMobile.jpg" style="height: 500px"></img>
<img src="DemoImages/TierlistMobileMinimal.jpg" style="height: 500px"></img>
</div>

## Features

### Creating a Tierlist
You can always create a new Tierlist, that is instantly stored locally. By default has tiers S to F. <br>
<div align="center">
<img src="DemoImages/G_CreateTierlist.gif" style="width: 700px"></img>
</div>

### Creating Items
While not in showcase mode, you can add items to any available tiers, they can have:
* Title
* Image link to be shown
* (Mobile only) Pick local image
* Tags (categories to be filtered)
* Notes (Description)
<br>

<div align="center">
<img src="DemoImages/G_CreateItem.gif" style="width: 700px"></img>
</div>

### Organizing Tierlist
You can drag and drop any item in any tier to another tier, or to another item, where it is then inserted. 
<div align="center">
<img src="DemoImages/G_Dragging.gif" style="width: 700px"></img>
</div>

### Tierlist Display Settings
Above the tierlist there are 3 switches:
* Showcase Mode: this removes the buttons for adding new items or tiers, and shows a tooltip of the items when hovered.
* Minimal Mode: makes the items smaller and forces the conventional tierlist display. Good when your tiers have many items.
* Text Only: forces all items to display only as their titles. Can be used as a reference if the images are not recognizable enough.
<div align="center">
<img src="DemoImages/G_DisplaySettings.gif" style="width: 700px"></img>
</div>

### Tierlist Filtering
In the tierlist header there is a search bar, where you can specify a filter. Then only the items within that filter are shown. <br> Filter functions:
* <b>Title Search:</b> Plain text filters by title. Ex: "Dark" shows all items with "Dark" in the title (case-insensitive)
* <b>Tag Search:</b> Text inside [brackets] filters by tags. Ex "[Dark]" shows all items with the "Dark" tag (case-insensitive)
* <b>AND operation:</b> combine filters by separating them with "+". Ex: "Dark+Souls", "[Movie]+[Pixar]", "Devil+[3D]"
* <b>NOT operation:</b> exclude matches by prefixing a filter with "-". Ex: "-Dark" "-[Movie]". When combining with other filters, remember to add them first. "[Movie]+-[Pixar]".
* <b>OR operation:</b> match any of multiple filters by separating them with ",". Ex: "Dark, Light" "[Movie],[Short]" "Dark, [Movie]"
<div align="center">
<img src="DemoImages/G_Filtering.gif" style="width: 700px"></img>
</div>

### Editing Tiers
With the "Edit Tiers" button, a menu shows up where you can change the name, color and position of each tier.
<div align="center">
<img src="DemoImages/G_TierEdit.gif" style="width: 700px"></img>
</div>

### Exporting Tierlists
With the "Export Tierlist" button, the JSON of the current tierlist gets copied to your clipboard, so it can be shared between people or between devices. <br> Additionally, theres an "Export All Tierlists" button that does the same but with all saved tierlists.
<div align="center">
<img src="DemoImages/G_Exporting.gif" style="width: 700px"></img>
</div>

### Importing Tierlists
With the "Import Tierlist" button, you can paste the JSON of one or multiple tierlists, that are parsed and saved to your data.
<div align="center">
<img src="DemoImages/G_Importing.gif" style="width: 700px"></img>
</div>

### Tierlist Managing
When the "Edit tierlists" switch is on, you can:
* Change order of the tierlists (with drag and drop)
* Change color of the tierlists
* Delete tierlists
<div align="center">
<img src="DemoImages/G_TierListManage.gif" style="width: 700px"></img>
</div>
