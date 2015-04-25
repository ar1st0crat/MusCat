# MusCat
<i>Music Catalogizer + MP3 ID tag parser + Radio</i>

The <b>MusCat</b> app (short for <b>Mus</b>ic <b>Cat</b>alog) is intended for all users who prefer to store their audio files locally and want to organize their music collections. The app stores data in a local database and provides a rich GUI functionality for viewing and editing all sorts of information regarding performers and their discography.

> Main features:
* you can easily "CRUD" performers, lineups, albums and songs using rich and intuitive GUI
* you can add all songs of any album in one click using the <i>Parse songlist</i>  button. The app will automatically walk through the entire directory of the album, parse each mp3 file in it and add it to the current album in the database.
* you can launch the special <i>MusCat Radio</i>  tool that will create a random playlist and play songs for you based on your filters (genre, duration, decade, etc.).

Since MusCat doesn't store multimedia data in a local database (neither it stores the relative paths to images and sounds) it uses several naming conventions to find necessary files in your local file system:
* In <i>Settings</i> menu you can specify the default paths |ROOTPATH_1|, |ROOTPATH_2|, etc. For instance, you may set the following paths: "F:\", "G:\", "G:\All\"
* The main photo of a performer should be located in the file of the following structure: |ROOTPATH|\|1st_letter_of_performer|\|Performer|\Picture\Photo.<i>ext</i>. The file extension is irrelevant. The app will try out each of the default root paths. For instance, MusCat may find the photo of a band Camel in such files as F:\C\Camel\Picture\photo.jpg, G:\All\C\Camel\Picture\photo.bmp
* The album covers of a performer should be located in the files of the following structure: |ROOTPATH|\|1st_letter_of_performer|\Performer\Picture\<i>coverID.ext</i>. The file extension is irrelevant. The ID part should coincide with the albumID stored in database - you may rename the file manually or press the <i>Set Picture</i> button and the app will automatically give the file a correct name and place it to the correct folder, for instance: F:\C\Camel\Picture\38.jpg or G:\All\C\Camel\Picture\126.jpg
* Sound files should be located in the folders of the following structure: |ROOTPATH|\|1st_letter_of_performer|\|Performer|\<i>AlbumFolder</i>. The last part of a path should contain the name of an album and the year of album's release in any combination. For instance, F:\C\Camel\2002 - A Nod And A Wink, F:\C\Camel\A Nod and a wink [2002]

In case of violation of the above-mentioned conventions nothing crucial happens - the only problem is that MusCat won't be able to find appropriate images and sounds and will replace them with built-in default files.

> Requirements:
* Windows Vista / Seven / 8
* .NET Framework 4.0 and higher
* MS SQL Server 2012
