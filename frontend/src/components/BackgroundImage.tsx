import { makeStyles } from '@material-ui/core';
import React from 'react';

export interface BackgroundImageProps {
  src?: string;
  alt?: string;
}

const useStyles = makeStyles({
  root: {
    display: 'block',
    position: 'absolute',
    width: '100%',
    height: '100%',
    zIndex: -100,
  },
});

export default function BackgroundImage(props: BackgroundImageProps) {
  const classes = useStyles();

  return <img {...props} className={classes.root} />;
}
