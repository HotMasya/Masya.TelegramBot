import React from 'react';
import BackgroundImage from '../components/BackgroundImage';
import AuthForm from '../components/AuthForm';
import CenteredContainer from '../components/CenteredContainer';
import endpoints from '../routing/endpoints';
import background from '../static/images/auth_background.jpg';
import { useTheme, useMediaQuery } from '@material-ui/core';

export default function AuthPage() {
  const theme = useTheme();
  const currentBreakpoint = theme.breakpoints.up('sm');
  const isUpMd = useMediaQuery(currentBreakpoint);

  return (
    <>
      {isUpMd && <BackgroundImage src={background} alt="background_image" />}
      <CenteredContainer>
        <AuthForm apiEndpoint={endpoints.auth} caption="Authorization" />
      </CenteredContainer>
    </>
  );
}
