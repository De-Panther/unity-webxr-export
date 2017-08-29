mergeInto(LibraryManager.library, {

	InitJavaScriptSharedArray: function(byteOffset, length) {
		JavaScriptSharedArray = new Float32Array(buffer, byteOffset, length);
	},

	InitJavaScriptSharedArrayButtons: function() {
		for(var i = 0; i < JavaScriptSharedArray.length; i++) {
			var button = document.createElement('button');
			button.style.cssText = 'position: absolute; top: ' + i*100 + ' px; left: 0px';
			button.index = i;
			button.innerHTML = i;
			button.onclick = function() { JavaScriptSharedArrayIncrement(this.index); }
				document.body.appendChild(button);
		}
	}
});