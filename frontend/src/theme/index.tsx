import { createTheme, CssBaseline, ThemeProvider } from '@material-ui/core';
import React, { useMemo } from 'react';
import GlobalStyles from './GlobalStyles';
import palette from './palette';

const ThemeConfig: React.FC = (props) => {
  const { children } = props;
  const theme = useMemo(() => {
    return createTheme({
      palette: {
        ...palette,
      },
    });
  }, []);

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <GlobalStyles />
      {children}
    </ThemeProvider>
  );
};

export default ThemeConfig;
