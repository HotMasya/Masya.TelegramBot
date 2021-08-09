import React from 'react';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import AuthPage from '../pages/AuthPage';
import HomePage from '../pages/HomePage';

const Navigation: React.FC = () => {
  const userAuthorized = false;
  return (
    <BrowserRouter>
      <Switch>
        <Route path="/">{userAuthorized ? <HomePage /> : <AuthPage />}</Route>
      </Switch>
    </BrowserRouter>
  );
}

export default Navigation;