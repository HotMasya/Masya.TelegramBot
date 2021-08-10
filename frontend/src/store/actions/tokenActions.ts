import { TokenModel } from "../../models/TokenModel";
import { createStandardAction } from "typesafe-actions";
import { TokenActionTypes } from "../action-types";

export const setToken = createStandardAction(TokenActionTypes.TOKEN_SET)<TokenModel>();
export const tokenError = createStandardAction(TokenActionTypes.TOKEN_ERROR)<Error>();
export const tokenRefresh = createStandardAction(TokenActionTypes.TOKEN_REFRESH)();
