# -*- coding: cp1251 -*-
import Tkinter, tkFileDialog
import xml.etree.ElementTree
import os, os.path
import shutil


# open file dialog for specifying paths.xml file
root = Tkinter.Tk()
root.withdraw()
filename = tkFileDialog.askopenfilename()

if filename == '':
    exit()

# parse xml
e = xml.etree.ElementTree.parse(filename).getroot()	
pathroots = [path.text for path in e.findall('path')]

# generate all possible combinations of paths
letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЭЮЯ1234567890"
paths = [ path + '/' + letter + '/' for letter in letters for path in pathroots ]

# leave only existing directories
paths = filter(lambda name: os.path.exists(name), paths)

# store the Picture folder path for each performer (if one exists)
subFolders = []
for path in paths:
    subFolders.append([os.path.join(path,x,"Picture") for x in os.listdir(path)\
                       if os.path.isdir(os.path.join(path,x)) and \
                          os.path.exists(os.path.join(path,x,"Picture"))])

# finally, add all files in Picture folders to the copylist
copylist = []
for i in range(len(paths)):
    for folder in subFolders[i]:
        for pic in filter(lambda name: os.path.isfile(os.path.join(paths[i],folder,name)),\
                          os.listdir(folder)):
            if pic[:pic.rindex('.')].isdigit()\
            or pic.startswith('photo.')\
            or pic.startswith('foto.'):
                copylist.append(os.path.join(paths[i],folder,pic))

# choose destination folder
destination = tkFileDialog.askdirectory()
if destination == '':
    exit()
    
# uncomment this check if it's necessary   
#if not os.path.exists( destination):
#    os.makedirs( destination )

# copy all images to destination folder
# (preserving relative structure)
print len(copylist)
for f in copylist:
    print f
    for p in pathroots:
        if f.startswith(p):
            newfile = f.replace(p, destination)
            newfiledir = os.path.split( newfile )[0]
            if not os.path.exists( newfiledir):
                os.makedirs( newfiledir )
            try:
                shutil.copy(f, newfiledir)
            except IOError:
                continue
