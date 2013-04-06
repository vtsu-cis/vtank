#!/usr/bin/python
# \file fullsync.py
# \brief A basic script for formatting the changelog to HTML
# \author Michael Smith
#
###########################################################################

import re

inFile = "changelog.txt"
outFile = "patchnotes.html"
outFileOld = "oldnotes.html"

#Read the file into buffer
inList = open(inFile, 'rU').readlines()

#Open write file;
global writeOut
writeOut = open(outFile, 'wb')
global versioncount
versioncount = 0

def version_format(line):
	global versioncount
	global writeOut
	versioncount=versioncount+1
	line = "<h2>%s</h2>" % (line)
	if versioncount is 2:
		writeOut.close()
		writeOut = open(outFileOld, 'wb')

	return line

def minus_format(line):
	count = 0
	for ch in line:
		if ch is '-':
			count=count+1
	
	line = "<h3>%s</h3>" % (line[count:])	
	return line

def hash_format(line):
	count = 0
        for ch in line:
		count=count+1
                if ch is '#':
			break

        line = "<h4>%s</h4>" % (line[count:])
        return line

def plus_format(line):
        count = 0
        for ch in line:
		count=count+1
                if ch is '+':
			break

        line = "<p>%s</p>" % (line[count:])
        return line

for line in inList:
	#Check line for a +, # or -
	plusmatch = re.compile('\+').search(line, 1)
	hashmatch = re.compile('\#').search(line, 1)
	minusmatch = re.compile('[\-\s]').search(line, 1)
	pattern = re.compile('Version*')
	versionmatch = pattern.match(line)
	
	#Perform different formatting actions for each case.
	if plusmatch:
		line=plus_format(line)
		writeOut.write(line)
	elif hashmatch:
		line=hash_format(line)
		writeOut.write(line)
	elif versionmatch:
		line=version_format(line)
		writeOut.write(line)
	elif minusmatch:
		line=minus_format(line)
		writeOut.write(line)

writeOut.close()
