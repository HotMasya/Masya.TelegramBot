import { createReducer } from "typesafe-actions";
import { RootAction } from "..";
import { TokenModel } from "../../models/TokenModel";
import * as actions from '../actions';

export type TokenState = {
    tokens?: TokenModel,
    error?: Error,
}

const initialState: TokenState = {
    tokens: {
        token: localStorage.getItem('x-access-token') as string,
    }
};

export const tokenReducer = createReducer<TokenState, RootAction>(initialState)
    .handleAction(actions.setToken, (state, action) => ({
        ...state,
        tokens: action.payload,
    }))
    .handleAction(actions.tokenError, (state, action) => ({
        ...state,
        error: action.payload
    }));