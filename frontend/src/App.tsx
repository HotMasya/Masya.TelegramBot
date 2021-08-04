import React from 'react';
import '@fontsource/roboto';
import ThemeConfig from './theme';
import Navigation from './routing/Navigation';

export default function App() {
  return (
    <ThemeConfig>
      <Navigation />
    </ThemeConfig>
  );
}
