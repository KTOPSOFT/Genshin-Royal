mergeInto(LibraryManager.library, {

    OpenWebURL: function(link)
    {
    	var url = Pointer_stringify(link);
        document.onmouseup = function()
        {
        	window.open(url);
        	document.onmouseup = null;
        }
    },
});