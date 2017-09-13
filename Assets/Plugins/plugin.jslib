mergeInto(LibraryManager.library, {
	InitJavaScriptSharedArray: function(byteOffset, length) {
		JavaScriptSharedArray = new Float32Array(buffer, byteOffset, length);
	},

	InitJavaScriptSharedArrayButtons: function() {
		for(var i = 0; i < JavaScriptSharedArray.length; i++) {
			var button = document.createElement('button');
			button.index = i;
			button.innerHTML = i;
			button.onclick = function() { JavaScriptSharedArrayIncrement(this.index); }
				document.body.appendChild(button);
		}
	}
});