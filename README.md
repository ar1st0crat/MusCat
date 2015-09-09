# MusCat
<i>Music Catalogizer + MP3 ID tag parser + Radio</i>

The <b>MusCat</b> app (short for <b>Mus</b>ic <b>Cat</b>alog) is intended for all users who prefer to store their audio files locally and want to organize their music collections. The app stores data in a local database and provides a rich GUI functionality for viewing and editing all sorts of information regarding performers and their discography.

> Main features:
* you can easily "CRUD" performers, lineups, albums and songs using rich and intuitive GUI
* you can add all songs of any album in one click using the <i>Parse mp3...</i>  button. The app will automatically walk through the entire directory of the album, parse each mp3 file in it and add it to the current album in the database.
* you can launch the special <i>MusCat Radio</i> tool that will create a random playlist and play songs for you based on your filters (genre, duration, decade, etc.).

Since MusCat doesn't store multimedia data in a local database (neither it stores the relative paths to images and sounds) it uses several naming conventions to find necessary files in your local file system:
* In <i>Settings</i> menu you can specify the default paths <em>|RootPath_1|, |RootPath_2|,</em> etc. For instance, you may set the following paths: <em>"F:", "G:", "G:\Others", "D:\Music"</em>
* The main photo of a performer should be located in the file of the following structure: <em>|ROOTPATH|\|1st_letter_of_performer|\|Performer's name|\Picture\Photo.<i>ext</i></em>. The file extension is irrelevant. The app will try out each of the default root paths. For instance, <b>MusCat</b> may find the photo of a band Camel in such files as <em>F:\C\Camel\Picture\photo.jpg</em>, <em>G:\All\C\Camel\Picture\photo.bmp</em>
* The album covers of a performer should be located in the files of the following structure: <em>|ROOTPATH|\\|1st_letter_of_performer|\Performer\Picture\\<i>coverID.ext</i></em>. The file extension is irrelevant. The ID part should coincide with the albumID stored in database - you may rename the file manually or press the <i>Load Image From File...</i> or <i>Load Image From Clipboard...</i> button and the app will automatically give the file a correct name and place it to the correct folder, for instance: <em>F:\C\Camel\Picture\38.jpg</em> or <em>G:\All\C\Camel\Picture\126.jpg</em>
* Sound files should be located in the folders of the following structure: <em>|ROOTPATH|\\|1st_letter_of_performer|\\|Performer|\\<i>AlbumFolder</i></em>. The last part of a path should contain the name of an album and the year of album's release in any combination. For instance, <em>F:\C\Camel\2002 - A Nod And A Wink</em>, <em>F:\C\Camel\A Nod and a wink [2002]</em>

In case of violation of the above-mentioned conventions nothing crucial happens - the only problem is that MusCat won't be able to find appropriate images and sounds and will replace them with built-in default files.

> Requirements:
* Windows Vista / Seven / 8
* .NET Framework 4.0 and higher
* MS SQL Server 2012

Main Window
![pic1](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/1.png)

MusCat Radio
![pic2](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/2.png)

Album window
![pic3](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/3.png)

Filters demo
![pic3](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/4.png)
