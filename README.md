# SouqScrapper
 
A C# console application used to scrape items data from souq.com website

The application gets the HTML content then gets the important data from it, After that, parses it to C# Objects using (HTML Agility Pack) from Nuget Package Manager.

The collected data stored in a file and the product's image is downloaded, All of that stored in 
` /Product/{Product Name} ` which will be created when the script runs.

### Used Packages

* HTML Agility Pack