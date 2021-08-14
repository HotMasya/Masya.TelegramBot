import React from 'react';
import '@fontsource/roboto';
import ThemeConfig from './theme';
import Navigation from './routing/Navigation';
import { Provider } from 'react-redux';
import { store } from './store';

const App: React.FC = () => (
  <ThemeConfig>
    <Provider store={store}>
      <Navigation />
    </Provider>
  </ThemeConfig>
);

export default App;
