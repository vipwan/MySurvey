import React, { useState, useEffect } from 'react';
import { Form, Input, Radio, Checkbox, InputNumber, Button, Card, message } from 'antd';
import { useNavigate, useParams } from 'react-router-dom';
import { surveyApi } from '../../services/api';

const SurveyAnswer = () => {
    const [form] = Form.useForm();
    const [survey, setSurvey] = useState(null);
    const [loading, setLoading] = useState(false);
    const [submitting, setSubmitting] = useState(false);
    const navigate = useNavigate();
    const { id } = useParams();

    useEffect(() => {
        fetchSurvey();
    }, [id]);

    const fetchSurvey = async () => {
        try {
            setLoading(true);
            const response = await surveyApi.getSurvey(id);
            setSurvey(response.data);
        } catch (error) {
            message.error('获取问卷信息失败');
        } finally {
            setLoading(false);
        }
    };

    const onFinish = async (values) => {
        try {
            setSubmitting(true);
            const answers = Object.entries(values).map(([questionId, value]) => {
                const question = survey.questions.find(q => q.id === questionId);
                let answer = {
                    questionId,
                };

                switch (question.type) {
                    case 1: // 单选题
                        answer.optionIds = [value];
                        break;
                    case 2: // 多选题
                        answer.optionIds = value;
                        break;
                    case 3: // 填空题
                        answer.content = value;
                        break;
                    case 4: // 评分题
                        answer.ratingValue = value;
                        break;
                }

                return answer;
            });

            await surveyApi.submitAnswer(id, {
                questionAnswers: answers,
            });

            message.success('提交成功');
            navigate('/survey-success');
        } catch (error) {
            message.error('提交失败');
        } finally {
            setSubmitting(false);
        }
    };

    if (loading || !survey) {
        return <div>加载中...</div>;
    }

    return (
        <div>
            <Card title={survey.title}>
                <div style={{ marginBottom: 24 }}>
                    <h3>问卷描述</h3>
                    <p>{survey.description || '暂无描述'}</p>
                </div>

                <Form
                    form={form}
                    layout="vertical"
                    onFinish={onFinish}
                >
                    {survey.questions.map(question => (
                        <Card key={question.id} style={{ marginBottom: 16 }}>
                            <Form.Item
                                name={question.id}
                                label={
                                    <span>
                                        {question.title}
                                        {question.isRequired && <span style={{ color: 'red' }}>*</span>}
                                    </span>
                                }
                                rules={[
                                    {
                                        required: question.isRequired,
                                        message: '请回答此问题',
                                    },
                                ]}
                            >
                                {renderQuestionInput(question)}
                            </Form.Item>
                        </Card>
                    ))}

                    <Form.Item>
                        <Button
                            type="primary"
                            htmlType="submit"
                            loading={submitting}
                            size="large"
                        >
                            提交问卷
                        </Button>
                    </Form.Item>
                </Form>
            </Card>
        </div>
    );
};

const renderQuestionInput = (question) => {
    switch (question.type) {
        case 1: // 单选题
            return (
                <Radio.Group>
                    {question.options.map(option => (
                        <Radio key={option.id} value={option.id}>
                            {option.content}
                        </Radio>
                    ))}
                </Radio.Group>
            );
        case 2: // 多选题
            return (
                <Checkbox.Group>
                    {question.options.map(option => (
                        <Checkbox key={option.id} value={option.id}>
                            {option.content}
                        </Checkbox>
                    ))}
                </Checkbox.Group>
            );
        case 3: // 填空题
            return <Input placeholder="请输入答案" />;
        case 4: // 评分题
            return (
                <InputNumber
                    min={1}
                    max={question.ratingMax || 5}
                    placeholder="请选择评分"
                />
            );
        default:
            return null;
    }
};

export default SurveyAnswer; 