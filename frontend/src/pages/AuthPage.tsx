import React, { Dispatch } from 'react';
import BackgroundImage from '../components/BackgroundImage';
import AuthForm from '../components/AuthForm';
import CenteredContainer from '../components/containers/CenteredContainer';
import background from '../static/images/auth_background.jpg';
import { useTheme, useMediaQuery } from '@material-ui/core';
import { SubmitHandler } from 'react-hook-form';
import { AuthModel } from '../models/AuthModel';
import { apiEndpoints } from '../routing/endpoints';
import { useDispatch, useSelector } from 'react-redux';
import { actions, RootAction } from '../store';
import RootState from '../store/reducers';

const AuthPage: React.FC = () => {
  const theme = useTheme();
  const currentBreakpoint = theme.breakpoints.up('sm');
  const isUpMd = useMediaQuery(currentBreakpoint);
  const dispatch = useDispatch<Dispatch<RootAction>>();
  const accountState = useSelector((state: RootState) => state.account);
  const onSubmit: SubmitHandler<AuthModel> = (model) => {
    if(!accountState.isPhoneSuccess){
      dispatch(actions.checkPhone(model.phone));
    }
    else if(!accountState.isCodeSuccess)
    {
      dispatch(actions.checkCode(model));
    }
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
