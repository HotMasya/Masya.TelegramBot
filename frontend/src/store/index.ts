import RootState, { rootReducer } from "./reducers";
import { applyMiddleware, createStore } from "redux";
import { createEpicMiddleware } from "redux-observable";
import RootAction from "./actions";
import rootEpic from "./epics";

const epicMiddleware = createEpicMiddleware<RootAction, RootAction, RootState>({});

const configureStore = (initialState?: RootState) => {
    const middlewares = [epicMiddleware];
    return createStore(rootReducer, initialState, applyMiddleware(...middlewares));
}

export const store = configureStore();
export * as actions from './actions';

epicMiddleware.run(rootEpic);
