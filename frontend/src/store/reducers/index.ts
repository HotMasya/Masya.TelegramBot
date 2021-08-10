import { combineReducers } from 'redux';
import { StateType } from 'typesafe-actions';
import accountReducer, { AccountState } from './accountReducer';

export const rootReducer = combineReducers({
    account: accountReducer,
});

type RootState = StateType<typeof rootReducer>;

export default RootState;
