// eslint-disable-next-line @typescript-eslint/no-var-requires
const path = require('path');
// eslint-disable-next-line @typescript-eslint/no-var-requires
const webpack = require('webpack');
// eslint-disable-next-line @typescript-eslint/no-var-requires
const HtmlWebpackPlugin = require('html-webpack-plugin');
const port = 80;
const host = '192.168.0.104';
const base = __dirname;
const envs = [];
const proxy = undefined;
module.exports = (env, { mode = 'development' }) => {
  const isDevelopmentMode = mode === 'development';
  const config = {
    mode,
    entry: {
      app: './src/index.tsx',
    },
    resolve: {
      extensions: ['.js', '.jsx', '.ts', '.tsx'],
    },
    module: {
      rules: [
        {
          test: /\.(svg|jpe?g)$/,
          use: {
            loader: 'url-loader',
          },
        },
        {
          test: /\.(js|jsx|tsx|ts)$/,
          exclude: /node_modules/,
          use: {
            loader: 'babel-loader',
            options: {
              presets: [
                [
                  '@babel/preset-env',
                  {
                    modules: false,
                  },
                ],
                '@babel/preset-react',
                '@babel/preset-typescript',
              ],
              plugins: [
                '@babel/plugin-syntax-jsx',
                '@babel/plugin-transform-react-jsx',
                '@babel/plugin-transform-react-display-name',
                '@babel/plugin-proposal-class-properties',
                '@babel/plugin-transform-runtime',
                '@babel/plugin-transform-arrow-functions',
                '@babel/plugin-syntax-dynamic-import',
                '@babel/plugin-proposal-object-rest-spread',
              ],
            },
          },
        },
        {
          test: /\.(css)$/,
          use: ['style-loader', 'css-loader'],
        },
      ],
    },
    output: {
      path: path.resolve(base, 'dist'),
      filename: '[name].[contenthash:8].js',
      publicPath: '/',
    },
    optimization: {
      mangleWasmImports: true,
      mergeDuplicateChunks: true,
      minimize: true,
      nodeEnv: 'production',
      splitChunks: {
        chunks: 'async',
        minSize: 100000,
        minRemainingSize: 0,
        maxSize: 244000,
        minChunks: 1,
        maxAsyncRequests: 30,
        maxInitialRequests: 10,
        automaticNameDelimiter: '~',
        enforceSizeThreshold: 200000,
        cacheGroups: {
          defaultVendors: {
            test: /[\\/]node_modules[\\/]/,
            priority: -10,
          },
          default: {
            minChunks: 2,
            priority: -20,
            reuseExistingChunk: true,
          },
        },
      },
    },
    plugins: [
      new webpack.DefinePlugin({
        'process.env.NODE_ENV': '"production"',
      }),
      new webpack.EnvironmentPlugin(envs),
      new HtmlWebpackPlugin({
        filename: path.resolve(base, 'dist/index.html'),
        template: path.resolve(base, 'src', 'index.html'),
        minify: {
          collapseWhitespace: true,
          removeComments: true,
          removeRedundantAttributes: true,
          removeScriptTypeAttributes: true,
          removeStyleLinkTypeAttributes: true,
          useShortDoctype: true,
          html5: true,
          minifyCSS: true,
          minifyJS: true,
          minifyURLs: true,
        },
      }),
    ],
  };

  /**
   * If in development mode adjust the config accordingly
   */
  if (isDevelopmentMode) {
    config.devtool = 'source-map';
    config.output.filename = '[name]/index.js';
    config.module.rules.push({
      loader: 'source-map-loader',
      test: /\.js$/,
      exclude: /node_modules/,
      enforce: 'pre',
    });
    config.plugins = [
      new webpack.DefinePlugin({
        'process.env.NODE_ENV': '"development"',
      }),
      new HtmlWebpackPlugin({
        filename: path.resolve(base, 'dist/index.html'),
        template: path.resolve(base, 'src', 'index.html'),
      }),
      new webpack.EnvironmentPlugin(envs),
    ];
    config.devServer = {
      contentBase: path.resolve(base, 'dist'),
      port,
      host,
      publicPath: '/',
      historyApiFallback: true,
      proxy,
      stats: {
        colors: true,
        hash: false,
        version: false,
        timings: true,
        assets: true,
        chunks: false,
        modules: false,
        reasons: false,
        children: false,
        source: false,
        errors: true,
        errorDetails: true,
        warnings: false,
        publicPath: true,
      },
    };
    config.optimization = {
      mangleWasmImports: true,
      mergeDuplicateChunks: true,
      minimize: false,
      nodeEnv: 'development',
    };
  }
  return config;
};
