import { combineReducers } from 'redux';
import { StateType } from 'typesafe-actions';
import accountReducer from './accountReducer';
import { tokenReducer } from './tokenReducer';

export const rootReducer = combineReducers({
  account: accountReducer,
  token: tokenReducer,
});

type RootState = StateType<typeof rootReducer>;

export default RootState;
