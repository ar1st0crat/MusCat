# MusCat

*Music Catalogizer + MP3 ID tag parser and web loader + Radio*

The **MusCat** app (short for **Mus**ic **Cat**alog) is intended for all users who prefer to store their audio files locally and want to organize their music collections. The app stores data in a local database and provides a rich GUI functionality for viewing and editing all sorts of information regarding artists and their discographies.

![pic1](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/1.png)

Examples of an album window

![pic3](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/3.png)


### Main features:

* you can easily add/edit/remove performers (artists), albums and songs (tracks) using rich and intuitive GUI.

* you can add all songs of any album to the database in one click using the *Parse mp3...*  button. The app will automatically walk through the entire directory of the album, parse each mp3 file in it and add it to the current album in the database. Or you can click the *Load songlist* button and **MusCat** will load the entire tracklist from the web (currently it takes this data from *discogs.com*).

* similarly, you can fill the bio of an artist manually or in one click using the *Load Bio...* button. The app will load and parse the corresponding information from internet (currently from *Last.fm* pages).

* you can launch the special *MusCat Radio* tool that will create a random playlist and play songs for you based on your mood.

* you can paste album covers and performer photos from clipboard or from a local file.


Since **MusCat** doesn't store multimedia data in a local database (neither it stores the relative paths to images and sounds) it simply uses several naming conventions to find necessary files in your local file system:

* In *Settings* menu you can specify the default paths where MusCat will look for music and images. For instance, you may set the following paths:

   - ```F:```
   - ```G:```
   - ```G:\Others```
   - ```D:\Music```, etc.


* The main photo of a performer should be located in the file having the following fullname:

```ROOTPATH / 1st_letter_of_performer's_name / Performer / Picture / Photo.ext```

or 

```ROOTPATH / Performer / Picture / Photo.ext```

(Personally I prefer to organize performer/artist folders alphabetically on my local drives). The file extension is irrelevant. The app will try out each of the default root paths. For instance, MusCat may find the photo of a band *Camel* in such files as

   - ```F:\C\Camel\Picture\photo.jpg```
   - ```G:\All\C\Camel\Picture\photo.bmp```
   - ```F:\Camel\Picture\photo.jpeg```
   - ```G:\All\Camel\Picture\photo.png```, etc.

* The album covers should be located in the files having the following fullname: 

```ROOTPATH \ 1st_letter_of_performer's_name \ Performer \ Picture \ ID.ext```

or

```ROOTPATH \ Performer \ Picture \ ID.ext```

The file extension is irrelevant. The ID part should coincide with the album ID stored in database - you may rename the file manually or press the *Load Image From File...* or *Load Image From Clipboard* button and the app will automatically give the file a correct name and place it to the correct folder, for instance:

   - ```F:\C\Camel\Picture\38.jpg```
   - ```G:\All\C\Camel\Picture\126.jpg```
   - ```F:\Camel\Picture\38.jpg```
   - ```G:\All\Camel\Picture\126.jpg```, etc.

* Sound files should be located in the folders at the path like this:

```ROOTPATH \ 1st_letter_of_performer's_name \ Performer \ AlbumFolder```

or 

```ROOTPATH \ Performer \ AlbumFolder```

The last part of a path should contain the name of an album and the year of album's release in any combination. For instance:

   - ```F:\C\Camel\2002 - A Nod And A Wink```
   - ```F:\C\Camel\A Nod and a wink [2002]```
   - ```F:\Camel\2002 - A Nod And A Wink```
   - ```F:\Camel\A Nod and a wink [2002]```, etc.

In case of violation of the above-mentioned conventions nothing crucial happens - the only problem is that MusCat won't be able to find appropriate images and sounds and will replace them with built-in default files or simply show a corresponding message.

For example, part of user's file system can look somewhat like this:

![fs](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/fs.png)

<hr/>


MusCat Radio

![pic2](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/2.png)

Stats window

![pic4](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/5.png)

Some screenshots from Angular client:

![ang1](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/angular1.png)

![ang2](https://github.com/ar1st0crat/MusCat/blob/master/Screenshots/angular2.png)


## Used libraries:

- [EntityFramework](https://www.nuget.org/packages/EntityFramework)
- [AutoMapper](http://automapper.org/)
- [Autofac](https://autofac.org/)
- [LiveCharts](https://lvcharts.net)
- [NAudio](https://naudio.codeplex.com)
- [TagLib](http://taglib.org)
