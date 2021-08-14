import { createReducer } from 'typesafe-actions';
import { RootAction } from '..';
import { TokenModel } from '../../models/TokenModel';
import * as actions from '../actions';

export type TokenState = {
  tokens?: TokenModel;
  error?: Error;
};

const initialState: TokenState = {
  tokens: {
    accessToken: localStorage.getItem('x-access-token'),
    refreshToken: localStorage.getItem('x-refresh-token'),
  },
};

export const tokenReducer = createReducer<TokenState, RootAction>(initialState)
  .handleAction(actions.setTokens, (state, action) => ({
    ...state,
    tokens: action.payload,
    error: undefined,
  }))
  .handleAction(actions.tokenError, (state, action) => ({
    ...state,
    error: action.payload,
  }))
  .handleAction(actions.tokenRefreshSuccess, (state, action) => ({
    ...state,
    error: undefined,
    tokens: {
      ...state.tokens,
      refreshToken: action.payload
    }
  }));
