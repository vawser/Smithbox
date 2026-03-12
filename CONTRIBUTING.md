# Contributions
<details>
  <summary>Guide </summary>
To make contributions to Smithbox, you will first need to setup a local repository so you can create pull requests.

## Fork the repository
You can do this by clicking the fork button at the top of this page. This creates a copy of this repository within your account.

<details>
  <summary>Screenshot</summary>
<img width="1086" height="190" alt="image" src="https://github.com/user-attachments/assets/ce2e361d-95b3-4267-ab97-0117e64bff53" />
</details>

## Clone the repository
You can do this by clicking on the green code button at the top of this page. Go to the 'Local' tab, and copy the web url from the HTTPs subtab.

<details>
  <summary>Screenshot</summary>
<img width="937" height="477" alt="image" src="https://github.com/user-attachments/assets/ee084073-87f0-4a1b-996c-81c8abc5360a" />
</details>

### Github Client
If you are using the Github Client, go to File menu and then clicking Clone Repository...

<details>
  <summary>Screenshot</summary>
<img width="277" height="238" alt="image" src="https://github.com/user-attachments/assets/18e6cfab-8c70-4f27-ace2-24f79171deb1" />
</details>

Enter the web url and choose a folder for the project.
<details>
  <summary>Screenshot</summary>
<img width="545" height="336" alt="image" src="https://github.com/user-attachments/assets/02d1b40e-c7aa-41af-b7f2-2e3702a0d9ff" />
</details>

 ### Git
 If you are using git in a terminal, run the following command:

 `git clone "https://github.com/vawser/Smithbox.git"`

## Create a branch
To create a pull request, you will want to first create a branch within your local repository. 

This allows you to isolate your changes from the main branch, and will prevent commits on the main branch overwriting your work.

### Github Client
You can create a new branch by going to the Branch menu and clicking New Branch...

<details>
  <summary>Screenshot</summary>
<img width="601" height="521" alt="image" src="https://github.com/user-attachments/assets/a193b466-70b6-42b9-b3b5-1ed725411dcd" />
</details>

### Git
You can create a branchw ith this command:

`git switch -c branch-name`

Replace branch-name with a suitable branch name.

## Making Changes
With your branch active, you can now change files within the Smithbox project and create commits. 

These commits will be associated with the branch and will form your pull request once you have finished working.

### Github Client
You can make commits easily within the client like so:

<details>
  <summary>Screenshot</summary>
<img width="559" height="259" alt="image" src="https://github.com/user-attachments/assets/8eb9a4c5-477c-4dae-82bb-2304b89f1bb8" />
</details>

You can then push your changes to the remote repository by clicking the Fetch origin button:

<details>
  <summary>Screenshot</summary>
<img width="248" height="77" alt="image" src="https://github.com/user-attachments/assets/1cf3d1da-03c5-4784-83ee-9b28f51072e2" />
</details>

### Git
You can make commits via a terminal like so:

`git commit -m "commit title"`

You can then push your changes to the remote repository like so:

`git push -u origin branch-name`

## Create a Pull Request
Go to the main Smithbox repository and click Pull Requests, and then click the New pull request button, and select your branch.
</details>

# Translation
Smithbox now supports translations for param meta data (such as row names, field names and descriptions, etc).

To add support for a new language (such as Chinese), you need to do the following:

## Add the Language
Edit the *Annotation Languages.json*, adding a new entry like so:

`{
    "Name": "Chinese",
    "Folder": "Chinese"
}`

If you are adding new row names, you also need to edit the *Row Import Languages.json*, including the same entry as above.

## Discrete Translation
### Row Names
The row name metadata can be found in *Assets/PARAM/<project type>/Param Row Names/<language>*. 

If you are adding a new language, you will need to create a new folder here with the name you defined in the JSON above. 

You can then add JSON files that define the row name data. I recommend copying over the existing English ones into your new folder and then editing them.

### Param Annotations
The same applies for field annotations, except they are in the *Param Annotations* folder.

### Table Names
The same applies for Table Names, except they are in the *Param Table Names* folder.

## Embedded Translation
### Enums
Enum metadata supports multiple languages too. You can open a enum JSON and add a new entry to the Names list, like so:

`{
          "Language": "English",
          "Text": "Point"
        }`

The language string needs to be the same as the string you defined for the Name property in the *Annotation Languages.json* file.

### Param Categories
The same applies for Param Categories.

### Field Layouts
The same applies for Param Categories.

















