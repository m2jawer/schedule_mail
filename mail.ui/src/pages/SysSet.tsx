import { ProForm,ProFormText,ProFormDigit } from '@ant-design/pro-components';
import { mail_get_settings, mail_save_settings } from '@/services/ant-design-pro/api';
import {
  UserOutlined,
} from '@ant-design/icons';
import { useIntl,FormattedMessage,useModel } from '@umijs/max';
import { message } from 'antd';
import React from 'react';

const SysSet: React.FC = () => {
  const intl = useIntl();
  const [form] = ProForm.useForm();

  const fetchMailSettings = async () => {
    var mail_set = await mail_get_settings();

    form.setFieldsValue({
      mail: mail_set.Mail,
      mail_pwd: mail_set.MailPwd,
      mail_pop3: mail_set.MailPop3,
      mail_port: mail_set.MailPort,
    });
  };

  return (
    <div
        style={{
          flex: '1',
          padding: '32px 0',
        }}
    >
      <ProForm name="sys_set_form"
        onFinish={async (paras)=>{
          var result = await mail_save_settings(paras);
          if(result.success) {
            message.success('保存成功');
          }
          else {
            message.error('保存失败');
          }
        }}
        onInit={fetchMailSettings}
        form = {form}
      >
      <div
        style={{
          minWidth: 250,
          maxWidth: '10vw',
        }}
      >
        <ProFormText
          name="mail"
          fieldProps={{
            size: 'large',
            prefix: <UserOutlined />,
          }}
          placeholder='自动发送邮件源'
          rules={[
            {
              required: true,
              message: (
                <FormattedMessage id="mail.empty"
                  defaultMessage="邮件源不可以为空!"
                />
              ),
            },
          ]}
        ></ProFormText>
      </div>
      <div
        style={{
          minWidth: 250,
          maxWidth: '10vw',
        }}
      >
        <ProFormText.Password
          name="mail_pwd"
          fieldProps={{
            size: 'large',
            prefix: <UserOutlined />,
          }}
          placeholder='邮件发送验证秘钥'
          rules={[
            {
              required: true,
              message: (
                <FormattedMessage id="mail.pwd.empty"
                  defaultMessage="验证秘钥不可以为空!"
                />
              ),
            },
          ]}
        />
      </div>
      <div
        style={{
          minWidth: 250,
          maxWidth: '10vw',
        }}
      >
        <ProFormText
          name="mail_pop3"
          fieldProps={{
            size: 'large',
            prefix: <UserOutlined />,
          }}
          placeholder='邮件代理服务器SMTP地址'
          rules={[
            {
              required: true,
              message: (
                <FormattedMessage id="smtp.empty"
                  defaultMessage="SMTP地址不可以为空!"
                />
              ),
            },
          ]}
        />
      </div>
      <div
        style={{
          minWidth: 250,
          maxWidth: '10vw',
        }}
      >
      <ProFormDigit
          name="mail_port"
          fieldProps={{
            size: 'large',
            prefix: <UserOutlined />,
          }}
          placeholder='邮件代理服务器SMTP端口'
          rules={[
            {
              required: true,
              message: (
                <FormattedMessage id="smtp.port.empty"
                  defaultMessage="SMTP端口不可以为空!"
                />
              ),
            },
          ]}
        />
      </div>
      </ProForm>
    </div>
  );
};

export default SysSet;
