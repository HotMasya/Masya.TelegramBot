import { User } from "../../models/User";
import { createReducer } from "typesafe-actions";
import { RootAction } from "..";
import * as actions from '../actions';

export type AccountState = {
    user?: User,
    isPhoneSuccess?: boolean,
    isCodeSuccess?: boolean,
    checkPhoneError?: Error,
    checkCodeError?: Error,
};

const initialState: AccountState = {
    isPhoneSuccess: false,
    isCodeSuccess: false,
};

const accountReducer = createReducer<AccountState, RootAction>(initialState)
    .handleAction(actions.setUser, (state, action) => ({
        ...state,
        user: action.payload
    }))
    .handleAction(actions.checkPhoneSuccess, (state) => ({
        ...state,
        isPhoneSuccess: true,
    }))
    .handleAction(actions.checkPhoneFailure, (state, action) => ({
        ...state,
        checkPhoneError: action.payload,
    }))
    .handleAction(actions.checkCodeSuccess, (state) => ({
        ...state,
        isCodeSuccess: true,
    }))
    .handleAction(actions.checkCodeFailure, (state, action) => ({
        ...state,
        checkCodeError: action.payload,
    }));

export default accountReducer;
