module.exports = {
  testEnvironment: 'jsdom',
  "preset": "ts-jest",
  verbose: false,
  forceExit: true,
  testNamePattern: "",
  watchAll: false,
  ci: true,
  rootDir: '.',
  testMatch: ["**/*.test.fs.js"],
  transform: {
    '\\.js$': ['babel-jest', { configFile: './_babel.config.json' }]
  },
  moduleNameMapper: {
    "\\.(css|less|scss|sss|styl)$": "<rootDir>/node_modules/jest-css-modules"
  },
  "transformIgnorePatterns": [
  ],
};
