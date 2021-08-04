import { createTheme, CssBaseline, ThemeProvider } from '@material-ui/core';
import React, { PropsWithChildren, useMemo } from 'react';
import GlobalStyles from './GlobalStyles';
import palette from './palette';

export interface ThemeConfigProps {}

export default function ThemeConfig(
  props: PropsWithChildren<ThemeConfigProps>,
) {
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
}
