import React, { Dispatch } from 'react';
import BackgroundImage from '../components/BackgroundImage';
import AuthForm from '../components/AuthForm';
import CenteredContainer from '../components/containers/CenteredContainer';
import background from '../static/images/auth_background.jpg';
import { useTheme, useMediaQuery } from '@material-ui/core';
import { SubmitHandler } from 'react-hook-form';
import { PhoneModel } from '../models/AuthModel';
import { apiEndpoints } from '../routing/endpoints';
import { useDispatch } from 'react-redux';
import RootAction from '../store/actions';
import { actions } from '../store';

const AuthPage: React.FC = () => {
  const theme = useTheme();
  const currentBreakpoint = theme.breakpoints.up('sm');
  const isUpMd = useMediaQuery(currentBreakpoint);
  const dispatch = useDispatch<Dispatch<RootAction>>();

  const onSubmit: SubmitHandler<PhoneModel> = (model) => {
    dispatch(actions.checkPhone(model.phone));
  }

  return (
    <>
      {isUpMd && <BackgroundImage src={background} alt="background_image" />}
      <CenteredContainer>
        <AuthForm onSubmit={onSubmit} apiEndpoint={apiEndpoints.checkPhone} caption="Authorization" />
      </CenteredContainer>
    </>
  );
}

export default AuthPage;
