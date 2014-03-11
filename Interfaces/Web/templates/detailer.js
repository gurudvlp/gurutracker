function detailer()
{
	
	this.cactive = 0;
	
	this.Render = function()
	{
		var maintmp = Templates.t("detailer/main.html");
		
		maintmp = Templates.Replace("[TITLE]", Track.title, maintmp);
		maintmp = Templates.Replace("[AUTHOR]", Track.author, maintmp);
		maintmp = Templates.Replace("[YEAR]", Track.year, maintmp);
		maintmp = Templates.Replace("[GENRE]", this.Genre, maintmp);
		maintmp = Templates.Replace("[COMMENTS]", this.Comments, maintmp);
		
		document.getElementById("page_content").innerHTML = maintmp;
	}
}
