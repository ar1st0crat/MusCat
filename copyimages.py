# -*- coding: utf-8 -*-

#####################################################################
#                                                                   #
# @author: ar1st0crat                                               #
#                                                                   #
# MusCat works with a local filesystem and uses naming conventions  #
# for displaying album covers and performer photos.                 #
#                                                                   #
# This is the helper script for saving all image files              #
# related to MusCat mediabase into separate directory.              #
# The script preserves the internal structure of all folders.       #
#                                                                   #
#####################################################################

import tkinter
from tkinter import filedialog
import xml.etree.ElementTree
import os
import os.path
import shutil


# open file dialog for specifying paths.xml file
root = tkinter.Tk()
root.withdraw()
filename = filedialog.askopenfilename(
                        title='Choose xml file with path settings:')
if filename == '':
    exit()

try:
    # parse xml
    e = xml.etree.ElementTree.parse(filename).getroot()	
    pathroots = [path.text for path in e.findall('path')]
except:
    print('Some error occured during xml file parsing...')
    exit()

# choose destination folder
destination = filedialog.askdirectory(
                            title="Choose the destination folder:")
if destination == '':
    exit()
    
if not os.path.exists(destination):
    os.makedirs(destination)


# add trailing separator to each path root if it's not there
# (for some reason, os.path.join('D:','dir') will return 'D:dir'
#  so 'D:' should become 'D:\' first)
pathroots = [path if path.endswith(os.sep) else path + os.sep 
             for path in pathroots]

# each path will contain a capital letter or a digit:
english = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'
russian = 'АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЭЮЯ'
digits = '0123456789'
letters = english + russian + digits

# generate all possible combinations of path roots
paths = [os.path.join(path, letter) for letter in letters 
                                    for path in pathroots]
# leave only existing directories
paths = filter(lambda name: os.path.exists(name), paths)

for path in paths:
    letter = os.path.basename(path)
    # generate the path of a destination root:
    # - extract only letter if the path does not contain any nested path
    #       for example, for 'D:\A' the root is 'A'
    # - keep the letter and the nested path, otherwise
    #       for example, for 'D:\Foo\A' the root is 'Foo\A'
    if path.index(os.sep) == path.rindex(os.sep):
        root = letter
    else:
        root = path[path.index(os.sep) + 1:]
    # iterate through all performers
    for performer in os.listdir(path):
        performerpath = os.path.join(path, performer)
        # iterate through all files in performer's Picture folder
        imagepath = os.path.join(performerpath, 'Picture')
        if os.path.exists(imagepath):
            for imagefile in os.listdir(imagepath):
                srcfile = os.path.join(imagepath, imagefile)
                if not os.path.isfile(srcfile):
                    continue
                # if the file contains only digits (album ID)
                # or starts with 'photo' or 'foto' (performer photo)
                # then copy it
                if  imagefile[:imagefile.rindex('.')].isdigit() or \
                    imagefile.startswith('photo.') or \
                    imagefile.startswith('foto.'):
                    # create the fullpath of a destination folder
                    destdir = os.path.join(destination, 
                                            root, 
                                            performer, 
                                            u'Picture')
                    # display progress in console
                    try:
                        print('copying... %s' % srcfile)
                    except UnicodeEncodeError:
                        print('copying... Could not decode some symbols')
                    
                    # copy file
                    if not os.path.exists(destdir):
                        os.makedirs(destdir)
                    try:
                        shutil.copy(srcfile, destdir)
                    except IOError:
                        continue
