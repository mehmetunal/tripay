const gulp = require('gulp');
const postcss = require('gulp-postcss');
const tailwindcss = require('@tailwindcss/postcss');
const rename = require('gulp-rename');
const { deleteAsync } = require('del');

const paths = {
  css: { src: 'Styles/app.css', dest: 'wwwroot/css', out: 'web.css' }
};

function clean() {
  return deleteAsync([`${paths.css.dest}/web.css`]);
}

function styles() {
  return gulp
    .src(paths.css.src)
    .pipe(postcss([tailwindcss()]))
    .pipe(rename('web.css'))
    .pipe(gulp.dest(paths.css.dest));
}

function watch() {
  gulp.watch(paths.css.src, styles);
}

exports.clean = clean;
exports.styles = styles;
exports.build = gulp.series(clean, styles);
exports.watch = watch;
exports.default = exports.build;
