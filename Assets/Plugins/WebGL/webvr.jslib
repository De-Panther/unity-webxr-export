(function () {
  'use strict';
  mergeInto(LibraryManager.library, {
    FinishLoading: function() {
      document.dispatchEvent(new CustomEvent('Unity', {detail: 'Ready'}));
    }
  });
})();