# MusCat

*Music Catalogizer + MP3 ID tag parser + Radio*

The **MusCat** app (short for **Mus**ic **Cat**alog) is intended for all users who prefer to store their audio files locally and want to organize their music collections. The app stores data in a local database and provides a rich GUI functionality for viewing and editing all sorts of information regarding performers and their discographies.

### Main features:

* you can easily "CRUD" performers, albums and songs using rich and intuitive GUI.

* you can add all songs of any album to the database in one click using the *Parse mp3...*  button. The app will automatically walk through the entire directory of the album, parse each mp3 file in it and add it to the current album in the database.

* you can launch the special *MusCat Radio* tool that will create a random playlist and play songs for you based on your mood.

* you can paste album covers from clipboard.

Since MusCat doesn't store multimedia data in a local database (neither it stores the relative paths to images and sounds) it uses several naming conventions to find necessary files in your local file system:

* In *Settings* menu you can specify the default paths where MusCat will look for music and images. For instance, you may set the following paths: ```F:```, ```G:```, ```G:\Others```, ```D:\Music```, etc.

* The main photo of a performer should be located in the file of the following structure:

```ROOTPATH \ 1st_letter_of_performer \ Performer's name \ Picture \ Photo.ext```

The file extension is irrelevant. The app will try out each of the default root paths. For instance, MusCat may find the photo of a band *Camel* in such files as ```F:\C\Camel\Picture\photo.jpg```, ```G:\All\C\Camel\Picture\photo.bmp```.

* The album covers of a performer should be located in the files of the following structure: 

```ROOTPATH \ 1st_letter_of_performer \ Performer \ Picture \ ID.ext```

The file extension is irrelevant. The ID part should coincide with the album ID stored in database - you may rename the file manually or press the *Load Image From File...* or *Load Image From Clipboard* button and the app will automatically give the file a correct name and place it to the correct folder, for instance: ```F:\C\Camel\Picture\38.jpg``` or ```G:\All\C\Camel\Picture\126.jpg```.

* Sound files should be located in the folders of the following structure:

```ROOTPATH \ 1st_letter_of_performer \ Performer \ AlbumFolder```

The last part of a path should contain the name of an album and the year of album's release in any combination. For instance, ```F:\C\Camel\2002 - A Nod And A Wink```, ```F:\C\Camel\A Nod and a wink [2002]```.

In case of violation of the above-mentioned conventions nothing crucial happens - the only problem is that MusCat won't be able to find appropriate images and sounds and will replace them with built-in default files or simply show a corresponding message.


Main Window

![pic1](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/1.png)

MusCat Radio

![pic2](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/2.png)

Album window

![pic3](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/3.png)

Filters demo

![pic3](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/4.png)
