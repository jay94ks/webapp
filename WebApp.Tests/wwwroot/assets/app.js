(function (window) {
    $.ajaxSetup({
        beforeSend: function (xhr) {
            xhr.setRequestHeader('X-Requested-With', 'WebApp/jQuery');
        }
    });

    const test = {
        template: '<div> test!! </div>'
    };

    /**, {
            path: '/:catchAll(.*)',
            component: NotFoundComponent,
            name: 'not-found'
        } */

    const app = new Vue({ router: router }).$mount('#app');
})(window);