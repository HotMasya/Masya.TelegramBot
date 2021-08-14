import RootState, { rootReducer } from './reducers';
import { applyMiddleware, compose, createStore } from 'redux';
import { createEpicMiddleware } from 'redux-observable';
import * as actions from './actions';
import rootEpic from './epics';
import { ActionType } from 'typesafe-actions';

export type RootAction = ActionType<typeof actions>;

const epicMiddleware = createEpicMiddleware<
  RootAction,
  RootAction,
  RootState
>();

const configureStore = (initialState?: RootState) => {
  const middlewares = [epicMiddleware];
  const enhancer = compose(applyMiddleware(...middlewares));
  return createStore(rootReducer, initialState, enhancer);
};

const store = configureStore();

epicMiddleware.run(rootEpic);

export { store, actions };
