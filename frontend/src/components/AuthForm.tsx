import {
  createStyles,
  makeStyles,
  TextField,
  Theme,
  Typography,
} from '@material-ui/core';
import React, { PropsWithChildren } from 'react';
import GradientButton from './GradientButton';
import GlassPaper from './GlassPaper';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { PhoneModel, phoneScheme } from '../models/AuthModel';

export interface AuthFormProps {
  caption?: string;
  apiEndpoint: string;
  className?: string;
}

const useStyles = makeStyles((theme: Theme) =>
  createStyles({
    root: {
      [theme.breakpoints.up('sm')]: {
        width: 400,
      },
      [theme.breakpoints.down('sm')]: {
        paddingTop: theme.spacing(4),
      },
    },
  }),
);

export default function AuthForm(props: PropsWithChildren<AuthFormProps>) {
  const { caption, className } = props;
  const classes = useStyles();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<PhoneModel>({
    resolver: yupResolver(phoneScheme),
  });

  function onSubmit(model: PhoneModel) {
    console.log(model.phone);
  }

  return (
    <GlassPaper className={classes.root + ' ' + className}>
      <Typography variant="h4" align="center">
        {caption}
      </Typography>
      <form onSubmit={handleSubmit(onSubmit)}>
        <TextField
          fullWidth
          type="text"
          id="phone"
          label="Phone"
          placeholder="+XXXXXXXXXXXX"
          margin="dense"
          error={errors.phone?.message != null}
          {...register('phone')}
        />
        <Typography variant="caption" color="error">
          {errors.phone?.message}
        </Typography>
        <GradientButton type="submit" fullWidth variant="contained">
          Authorize
        </GradientButton>
      </form>
    </GlassPaper>
  );
}
