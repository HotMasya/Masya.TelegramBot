import React from 'react';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import AuthPage from '../pages/AuthPage';
import HomePage from '../pages/HomePage';
import { useSelector } from 'react-redux';
import { useDispatch } from 'react-redux';
import RootState from 'src/store/reducers';
import { actions, RootAction } from '../store';
import { Dispatch } from 'redux';
import { TokenModel } from '../models/TokenModel';

const Navigation: React.FC = () => {
  const accountState = useSelector((state: RootState) => state.account);
  // const dispatch = useDispatch<Dispatch<RootAction>>();

  // const accessToken = localStorage.getItem('x-access-token');
  // if(!accountState.userError && accessToken && !accountState.user)
  // {
  //   const tokenModel: TokenModel = {
  //     accessToken
  //   };

  //   dispatch(actions.getUser(tokenModel));
  // }
  
  return (
    <BrowserRouter>
      <Switch>
        <Route path="/">{accountState.user ? <HomePage /> : <AuthPage />}</Route>
      </Switch>
    </BrowserRouter>
  );
}

export default Navigation;