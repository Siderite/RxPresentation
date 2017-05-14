'use strict';

var Observer = require('./observer');

Observer.addToObject({
  create: require('./observer/create')
});

var Observable = require('./observable');

Observable.addToObject({
  catch: require('./observable/catch'),
  concat: require('./observable/concat'),
  create: require('./observable/create'),
  empty: require('./observable/empty'),
  from: require('./observable/from'),
  fromArray: require('./observable/fromarray'),
  fromCallback: require('./observable/fromcallback'),
  fromEvent: require('./observable/fromevent'),
  fromEventPattern: require('./observable/fromeventpattern'),
  fromNodeCallback: require('./observable/fromnodecallback'),
  fromPromise: require('./observable/frompromise'),
  interval: require('./observable/interval'),
  just: require('./observable/just'),
  merge: require('./observable/merge'),
  never: require('./observable/never'),
  of: require('./observable/of'),
  range: require('./observable/range'),
  throw: require('./observable/throw')
});

Observable.addToPrototype({
  catch: require('./observable/catch'),
  combineLatest: require('./observable/combinelatest'),
  concat: require('./observable/concat'),
  concatAll: require('./observable/concatall'),
  debounce: require('./observable/debounce'),
  distinctUntilChanged: require('./observable/distinctuntilchanged'),
  filter: require('./observable/filter'),
  finally: require('./observable/finally'),
  flatMap: require('./observable/flatMap'),
  flatMapLatest: require('./observable/flatmaplatest'),
  map: require('./observable/map'),
  merge: require('./observable/merge'),
  mergeAll: require('./observable/mergeall'),
  scan: require('./observable/scan'),
  skip: require('./observable/skip'),
  skipUntil: require('./observable/skipuntil'),
  switch: require('./observable/switch'),
  take: require('./obserable/take'),
  takeUntil: require('./observable/takeuntil'),
  tap: require('./observable/tap'),
  zip: require('./observable/zip')
});

var Rx = {
  BinaryDisposable: require('./binarydisposable'),
  CompositeDisposable: require('./compositedisposable'),
  Disposable: require('./disposable'),
  NAryDisposable: require('./narydisposable'),
  SerialDisposable: require('./serialdisposable'),
  SingleAssignmentDisposable: require('./singleassignmentdisposable'),

  Scheduler: require('./scheduler'),

  Observer: Observer,
  Observable: Observable,

  AsyncSubject: require('./asyncsubject'),
  BehaviorSubject: require('./behaviorsubject'),
  ReplaySubject: require('./replaysubject'),
  Subject: require('./subject')
};

module.exports = Rx;
